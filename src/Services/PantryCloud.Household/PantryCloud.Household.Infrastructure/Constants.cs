namespace PantryCloud.Household.Infrastructure;

public static class Constants
{
    public const string HouseholdInvitationEmailSubject = "Invitation to join a household";

    public const string HouseholdInvitationEmailBodyTemplate = """
                                                              <p>Hello,</p>
                                                              <p>You have been invited to join a household on PantryCloud. Click the link below to accept:</p>
                                                              <p><a href="{0}">Accept invitation</a></p>
                                                              <p>This link expires in {1} minutes.</p>
                                                              """;
}
