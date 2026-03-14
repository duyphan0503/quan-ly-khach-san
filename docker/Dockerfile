# docker/Dockerfile
FROM ubuntu:24.04 

# install basic utilities needed for later steps
RUN apt-get update -y \
 && apt-get install -y --no-install-recommends \
        curl \
        gnupg \
        ca-certificates \
        apt-transport-https \
 && rm -rf /var/lib/apt/lists/*

# …other preparation (apt‑get update/other packages)…

# install gcloud SDK
RUN echo "deb [signed-by=/usr/share/keyrings/cloud.google.gpg] \
        https://packages.cloud.google.com/apt cloud-sdk main" \
        | tee -a /etc/apt/sources.list.d/google-cloud-sdk.list \
 && curl https://packages.cloud.google.com/apt/doc/apt-key.gpg \
        | gpg --dearmor -o /usr/share/keyrings/cloud.google.gpg \
 && apt-get update -y \
 && apt-get install google-cloud-cli -y \
 && rm -rf /var/lib/apt/lists/*

# …remaining Dockerfile content…