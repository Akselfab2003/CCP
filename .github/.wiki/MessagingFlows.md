# Messaging Flows

This document describes the messaging flows for the application. It covers the different types of messages, their formats, and how they are processed within the system.

```mermaid
flowchart LR
    Customer["Customer"]
    EmailService["Email Service"]
    CustomerService["Customer Service"]
    TicketService["Ticket Service"]
    Exists{"Customer Exists?"}

    Customer -->|Sends Email| EmailService
    EmailService -->|Check if Customer Exists| CustomerService
    CustomerService --> Exists
    Exists --> Yes --> | Return Customer ID | EmailService
    Exists --> No -->|Create Customer & Return ID| EmailService
    EmailService -->|Create Ticket with Email Content & Customer ID| TicketService

```


### Supporter Replies To Ticket created from Email

When a supporter replies to a ticket that was created from an email, the following flow occurs:
```mermaid
flowchart LR
    Supporter["Supporter"]
    TicketService["Ticket Service"]
    EmailService["Email Service"]
    Customer["Customer"]

    Supporter -->|Replies to Ticket| TicketService
    TicketService -->|Check if Ticket was created from Email| EmailService
    EmailService -->|Send Reply to Customer's Email| Customer
```

### Customer Replies To Supporter Reply from Ticket created from Email

When a customer replies to a supporter's reply, the following flow occurs:
```mermaid
flowchart LR
    Customer["Customer"]
    EmailService["Email Service"]
    TicketService["Ticket Service"]
    Supporter["Supporter"]

    Customer -->|Replies to Supporter's Email| EmailService
    EmailService -->|Check if Reply is Associated with a Ticket| TicketService
    TicketService -->|Send Reply to Supporter| Supporter
```


