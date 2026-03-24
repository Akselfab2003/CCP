```mermaid
sequenceDiagram
    participant C as Customer
    participant TS as Ticket UI
    participant DB as Ticket Service
    participant NS as Notification Service

    C->>TS: Open "Create Ticket" page
    C->>TS: Submit own details, subject, description, priority & status
    TS->>TS: Validate required fields
    alt Validation failed
        TS-->>C: Error: Missing or invalid fields
    else Validation passed
        TS->>TS: Assign unique ticket ID & timestamp
        TS->>DB: Save ticket record
        alt Database error
            DB-->>TS: Save failed
            TS-->>C: Error: Could not save ticket
        else Save successful
            DB-->>TS: Ticket saved
            TS->>TS: Add ticket to open queue
            TS->>NS: Send confirmation to customer & notify support team
            NS-->>C: Confirmation notification sent
            TS-->>C: Ticket visible in own tickets overview
        end
    end
```