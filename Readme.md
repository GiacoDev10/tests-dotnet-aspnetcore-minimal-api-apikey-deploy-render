# Minimal ASP.NET Core API – Render.com Deployment Training

This repository contains a training project designed to practice the deployment workflow of a **Minimal API built with ASP.NET Core** on **Render.com**. The project integrates **OpenAPI** support and uses **Scalar** to enhance API documentation and developer experience.

## 🎯 Purpose of the Project
The goal is to provide a lightweight, reproducible setup for experimenting with:
- Building a Minimal API in ASP.NET Core  
- Documenting the API using OpenAPI + Scalar  
- Deploying containerized applications on Render.com  
- Managing sensitive configuration values such as API keys  
- Understanding best practices for Docker-based cloud deployments  

## 🧩 Key Features
- **Minimal API architecture** using ASP.NET Core  
- **OpenAPI/Swagger** generation for endpoint documentation  
- **Scalar UI** for modern, interactive API exploration  
- **API Key authentication** for protected endpoints  
- **Dockerized deployment** with attention to secure environment variable handling  
- **Render.com-ready configuration** for seamless cloud hosting  

## 🔐 Security Considerations
Because the API relies on an API key, the project emphasizes:
- Avoiding hard‑coded secrets  
- Using **Render environment variables** to store sensitive data  
- Ensuring Docker images do not expose private configuration  
- Following secure deployment patterns for public cloud platforms  

## 🚀 Deployment Workflow
The repository demonstrates the full deployment pipeline:
1. Build and containerize the Minimal API using Docker  
2. Push the image or connect the repository to Render  
3. Configure environment variables (API key, ports, etc.)  
4. Deploy and validate the API using Scalar/OpenAPI in the cloud  

## 📁 Repository Structure
RestFullMinimalApi-ApiKey/
├── 📂 RestFullApiKey/            # Main Project (Minimal API) 
│   └── RestFullApiKey.csproj
├── 📂 RestFullApiKeyTests/       # Unit Test Project
│   └── RestFullApiKeyTests.csproj
├── RestFullApiKey.slnx           # Visual Studio Solution File
└── Readme.md                     # Project Documentation

## 📦 Technologies Used
- **ASP.NET Core Minimal API**  
- **OpenAPI / Swagger**  
- **Scalar**  
- **Docker**  
- **Render.com**  

## 📝 Summary
This repository serves as a practical sandbox for learning how to deploy secure, documented, containerized ASP.NET Core Minimal APIs on Render.com. It is ideal for developers who want hands‑on experience with cloud deployment pipelines, API documentation tooling, and secure configuration management.

