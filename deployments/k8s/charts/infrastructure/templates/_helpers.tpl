{{/* Chart name and version for labels. */}}
{{- define "infrastructure.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/* Common labels applied to all infrastructure resources. */}}
{{- define "infrastructure.labels" -}}
helm.sh/chart: {{ include "infrastructure.chart" . }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/part-of: pantry-cloud
{{- end }}
