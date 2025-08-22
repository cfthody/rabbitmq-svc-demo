# POC RabbitMQ technical documentation

## Overview
Until version 9.X, the Data Acquisition Service (renamed in version 10.0) had an in-process message bus to handle the load received by the service in an event-based architecture.

In version 10, this in-process message bus shall be externalized to avoid loss of messages once the service is restarted.

## Goal
Setup RabbitMQ broker on your local environment and implement messages exchange between a producer console application and an RESTfull API named Employee Management Service (EMS).

It should also be possible to audit these operation in a log system that persist the operation in a database.

To keep a history of the executing operations in your database, all the executing CQS commands must be stored in a new database called EventStoreDB. The main purpose of it is to be able to replicate your database in the case of a disaster.

## Functional Requirements
- Implement an API to manage Employee and name it as Employ Management System (EMS)
- Through the API we must be able to:
    - Related to employee:
        - Create a new employee
        - Update an employee (Partially and totally)
        - List all employees
        - Have the details of an employee
        - Delete an employee
    - Related to Roles and Platoons
        - List all roles/platoons
        - Add a new role/platoon
        - Delete a role/platoon
- EMS will act as Consumer and Producer
- The console application will produce message to create an employee or a list of employees
- The environment must be fully functional within docker
- Notification event should be registered when an employee is created (publish and register in log the consumption of message)
- Notification event should be registered when an employed is fired (validate using rabbit if message was registered properly)
- topic for message inboun to the ems
- topic for message outbound from ems
- ems consume inbound topic, create employee, and them produce a new message with information of the newly created employee
- use backgrund service to update isactive flag form Fired employee

## Technical Requirements
- Create the API using .NET Core version >= 8.0.0 in detriment of the
- Create an image in docker for EMS System, and instantiate RabbitMQ, PostgreSQL and pgAdmin4 server containers
- Build a docker-compose with your environment and make the containers talk to each other
- Producer should serialize message in JSON format and write them to the appropriate topics. Others formats such as XML, Bynary and text should also be possible.
- Use [Bogus](https://github.com/bchavez/Bogus) a helper to generate multiple Employees in the console application
- The data must be stored PostgreSQL or In-Memory database.
- UT and Integration testing
- use design pattern to create consumer without changing code, only extending
- You must request your EMS for the possible platoons and roles, and generate a random new employee with those values. 
- The process of updating the ExitDate must be executed periodically
- The process of updating the ExitDate must be optimized
- Use fluent validations to validate requests

## Testing scenarios
- Usage of rabbitmq management to create a single employee.
- Usage of the console application to create at least 1000 employees.
- Usage of rabbitmq to analyze the EMS production content.
- Update the IsActive field via /PATCH
- Manually Trigger the job via Hangfire UI.
- Automatic periodic execution of the job.
- Store all commands and related data in your new database
    - Give a time-based sorting order to your events to replicate them in the same order.

## Assumptions

| Assumption | Description |
| :--------- | :---------- |
| AS01       |             |
| AS02       |             |
| AS03       |             |



## Out of scope

TBD

## Open points

* [ ] Segregate your system using Commands and Queries, and use a MediatR to mediate the execution of these requests.

## State of the art

`<Description on the current state of the system>`

## Proposed Solution