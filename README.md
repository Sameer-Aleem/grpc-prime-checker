gRPC Prime Checker

Introduction

This project is a demonstration of a gRPC-based service where a client sends rapid requests to a server to check if a number is prime. The server validates the numbers and sends back the results while keeping track of the most frequently requested prime numbers. This README provides a detailed look into the architecture, and scaling considerations of the application.

Setup and Run

Prerequisites:

.NET Core SDK 3.1 or later
gRPC tools for .NET Core

Steps:

1. Clone the repository:

git clone https://github.com/YOUR_USERNAME/grpc-prime-checker.git
cd grpc-prime-checker

2. Navigate to the server and client directories and run them:

Server:

cd PrimeServer
dotnet run

Client:

cd PrimeClient
dotnet run

Development Process

Client:

The client is designed to send a consistent stream of 10000 requests per second to the server. Each request contains a random number between 1 and 1000. The choice of sending such a high volume of requests is to simulate a high throughput scenario, testing the efficiency and reliability of the gRPC service.

A critical aspect of the client is the verification of responses. Given the high volume of requests, it's crucial to ensure that each request receives a corresponding response. If there's a mismatch between sent requests and received responses, a warning is displayed, highlighting potential network or server issues.

Server:

On the server side, the responsibility is to validate if the incoming number is prime. The prime validation is achieved through an optimized algorithm that reduces the number of division checks needed, making it relatively faster and efficient.

The server also keeps track of valid prime numbers that are requested. A concurrent dictionary is employed for this purpose, ensuring thread safety given the multithreaded nature of incoming requests. Every second, the server displays the top 10 most frequently validated prime numbers and the total number of received messages.

Scaling Solution:

The application might need to scale beyond a single server instance. Here's a proposed strategy:

1. Containerization with Docker: Package the server application as a Docker container, allowing for easy distribution and deployment.

2. Kubernetes for Orchestration: Deploy the containers on a Kubernetes cluster. Kubernetes will manage the lifecycle of server instances, scaling up or down based on the demand.

3. Load Balancer: Use a Load Balancer like nginx to distribute incoming client requests evenly across server instances. This ensures that no single server is overwhelmed with too many requests.

4. State Management: Since the application currently operates in-memory, when scaling out, the state would be local to each instance. Consider using a distributed cache like Redis for centralized state management if cross-instance analytics is needed.

Testing:

For integration testing:

1. Mock the client requests and send them to the server.
2. Validate the server's response against expected outcomes.
3. Measure the time taken for the round trip and ensure it's within acceptable limits.
4. Test edge cases, such as sending non-numeric values, very large numbers, or sending requests at a higher frequency than expected.
5. Check the server's ability to handle multiple simultaneous client connections.
