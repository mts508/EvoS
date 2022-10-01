FROM alpine


# Install GIT
RUN apk add --no-cache git

# Install CURL
RUN apk add curl --no-cache

# Install BASH
RUN apk add bash icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib --no-cache

# Clone repo
RUN git clone https://github.com/mts508/EvoS.git

# Install dotnet
RUN curl https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh >> dotnet-install.sh
RUN chmod +x dotnet-install.sh 
RUN ./dotnet-install.sh --channel 3.1 --install-dir /usr/share/dotnet

# Compile Server
WORKDIR /EvoS
RUN /usr/share/dotnet/dotnet publish --configuration Release --output /EvosServer
RUN /usr/share/dotnet/dotnet nuget locals --clear all

# Remove repository folder (without chnaging the workdir we cannot erase the folder because we are in it) 
WORKDIR /
RUN rm -rf /EvoS

 
ENTRYPOINT /EvosServer/EvosServer