# Task Management System

## Overview

Task Management System developed as an exam project for The Database for Developers Exam.

The project consists of three independent services:

- TaskService - Manages task CRUD operations with PostgreSQL features
- NotificationService - Handles real-time notifications using Redis caching and PostgreSQL for persistence
- AnalyticsService - Tracks task events with MongoDB's schema and aggregates metrics

Each service is built using Clean Architecture principles, implements the CQRS pattern with MediatR, and communicates via RabbitMQ event-driven messaging. The entire system is containerized with Docker.
