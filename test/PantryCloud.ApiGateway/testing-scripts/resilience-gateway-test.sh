#!/bin/bash

# === CONFIGURATION ===
GATEWAY_URL="http://localhost:5050"
HEALTH_ENDPOINT="/health"
HOUSEHOLD_CONTAINER_NAME="pantry-household" 
HOUSEHOLD_ROUTE="/api/household/api/households"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}=== PantryCloud Resilience Test Suite ===${NC}"
echo -e "Target: $GATEWAY_URL"

# ---------------------------------------------------------
# 1. RESILIENCE / HEALTH CHECK TEST (Functional Test)
# ---------------------------------------------------------
echo -e "\n${YELLOW}--- 1. TEST: Resilience & Health Checks ---${NC}"

echo "Checking service availability..."
INITIAL_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$GATEWAY_URL$HOUSEHOLD_ROUTE")

if [ "$INITIAL_STATUS" == "429" ]; then
    echo -e "${RED}You are Rate Limited from a previous test!${NC}"
    echo "Please wait 60 seconds and try again."
    exit 1
elif [[ "$INITIAL_STATUS" =~ ^(200|401|405)$ ]]; then
    echo -e "${GREEN}Service is available (Status: $INITIAL_STATUS).${NC}"
else
    echo -e "${RED}TEST ABORTED: Service not responding correctly (Status: $INITIAL_STATUS).${NC}"
    echo "Check if Docker is running and the port is correct."
    exit 1
fi

echo -e "\n${BLUE}ACTION: STOP THE HOUSEHOLD CONTAINER!${NC}"
echo "Command: docker stop $HOUSEHOLD_CONTAINER_NAME"
echo "Press ENTER once the container is stopped..."
read

echo "Testing Gateway reaction (expecting 503 Service Unavailable)..."
FOUND_503=false
for i in {1..10}; do
    STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$GATEWAY_URL$HOUSEHOLD_ROUTE")
    echo "Request $i -> Status: $STATUS"
    
    if [ "$STATUS" == "503" ]; then
        echo -e "${GREEN}[PASS] Gateway detected failure (Returned 503).${NC}"
        FOUND_503=true
        break
    fi
    sleep 1
done

if [ "$FOUND_503" = false ]; then
    echo -e "${RED}Warning: Gateway did not return 503. Health Check might be slow or configured incorrectly.${NC}"
fi

echo -e "\n${BLUE}ACTION: START THE CONTAINER!${NC}"
echo "Command: docker start $HOUSEHOLD_CONTAINER_NAME"
echo "Press ENTER once the container is started..."
read

echo "Waiting for recovery (max 20s)..."
RECOVERED=false
for i in {1..20}; do
    STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$GATEWAY_URL$HOUSEHOLD_ROUTE")
    
    # If we get 401 (Unauthorized) or 405 (Method Not Allowed), the service is BACK.
    if [[ "$STATUS" =~ ^(200|401|405)$ ]]; then
         echo -e "${GREEN}[PASS] Service has recovered! (Status: $STATUS)${NC}"
         RECOVERED=true
         break
    fi
    echo -n "."
    sleep 1
done

if [ "$RECOVERED" = false ]; then
    echo -e "${RED}[FAIL] Gateway did not recover within 20 seconds.${NC}"
    exit 1
fi

# ---------------------------------------------------------
# 2. RATE LIMITING TEST (Destructive test last)
# ---------------------------------------------------------
echo -e "\n${YELLOW}--- 2. TEST: Rate Limiting (Stress Test) ---${NC}"
echo "Sending 110 requests rapidly (Limit is 100)..."

SUCCESS_COUNT=0
BLOCKED_COUNT=0

for i in {1..110}; do
    STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$GATEWAY_URL$HEALTH_ENDPOINT")
    if [ "$STATUS" == "200" ]; then 
        ((SUCCESS_COUNT++))
    elif [ "$STATUS" == "429" ]; then 
        ((BLOCKED_COUNT++))
        echo -n "x" 
    else 
        echo -n "." 
    fi
done

echo -e "\nResults: OK=$SUCCESS_COUNT | Blocked=$BLOCKED_COUNT"

if [ $BLOCKED_COUNT -gt 0 ]; then
    echo -e "${GREEN}[PASS] Rate limiting works. Gateway blocked requests.${NC}"
else
    echo -e "${RED}[FAIL] Rate limiting failed. No requests were blocked.${NC}"
fi

echo -e "\n${BLUE}Test Complete.${NC}"