# Stage 1: Build the application
#FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
#WORKDIR /app

# Copy the application source code
#COPY . .

# Build and publish the application
#RUN dotnet publish -c Release -o out

# Stage 2: Create a runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
WORKDIR /app
# Copy the published application from the build stage
COPY /out .
# Expose the port and specify the command to run the application
EXPOSE 80
CMD ["dotnet", "HelloWorldApp.dll"]

