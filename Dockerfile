FROM alpine

# This image builds the last version of master from the github repo
# create the image with -p 6050:6050/tcp -p 6060:6060/tcp
# optionally, add --no-cache on build to avoid skipping the git clone step

# Install deps
RUN apk add --no-cache git curl bash icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib && \
	git clone https://github.com/mts508/EvoS.git && \
	curl https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh >> dotnet-install.sh && \
	chmod +x dotnet-install.sh && \
	./dotnet-install.sh --channel 3.1 --install-dir /usr/share/dotnet

# Compile Server
WORKDIR /EvoS
RUN /usr/share/dotnet/dotnet publish --configuration Release --output /EvosServer && \
	/usr/share/dotnet/dotnet nuget locals --clear all

# Remove repository folder (without chnaging the workdir we cannot erase the folder because we are in it) 
WORKDIR /
RUN rm -rf /EvoS

ENTRYPOINT /EvosServer/EvosServer