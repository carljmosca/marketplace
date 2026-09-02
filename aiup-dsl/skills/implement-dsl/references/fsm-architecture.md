<!--
Copyright 2025-2026 Simon Martinelli and the AI Unified Process contributors.
Part of the AI Unified Process — https://unifiedprocess.ai
Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE.
-->

# Lightweight Java 21 Finite State Machine Architecture

This reference guides the creation of zero-dependency, idiomatic Java 21 finite state machines utilizing sealed types,
records, and pattern matching.

## State Representation

Model all states as a sealed hierarchy. Each state record carries its own state-specific context data:

```java
public sealed interface OrderState permits InitialState, DraftState, SubmittedState, ApprovedState, CancelledState {
    String name();
    List<String> validTransitions();
}

public record InitialState() implements OrderState {
    @Override public String name() { return "INITIAL"; }
    @Override public List<String> validTransitions() { return List.of("CREATE_ORDER"); }
}

public record DraftState(String orderId, String customerId, List<OrderItem> items) implements OrderState {
    @Override public String name() { return "DRAFT"; }
    @Override public List<String> validTransitions() { return List.of("ADD_ITEM", "SUBMIT_ORDER", "CANCEL_ORDER"); }
}

public record SubmittedState(String orderId, String customerId, List<OrderItem> items, Instant submittedAt) implements OrderState {
    @Override public String name() { return "SUBMITTED"; }
    @Override public List<String> validTransitions() { return List.of("APPROVE_ORDER", "REJECT_ORDER", "CANCEL_ORDER"); }
}

public record ApprovedState(String orderId, String customerId, List<OrderItem> items) implements OrderState {
    @Override public String name() { return "APPROVED"; }
    @Override public List<String> validTransitions() { return List.of(); } // Terminal
}

public record CancelledState(String orderId, String reason) implements OrderState {
    @Override public String name() { return "CANCELLED"; }
    @Override public List<String> validTransitions() { return List.of(); } // Terminal
}
```

## Event Representation

Model domain commands/events as a sealed event interface:

```java
public sealed interface OrderEvent permits CreateOrderEvent, AddItemEvent, SubmitOrderEvent, CancelOrderEvent {}

public record CreateOrderEvent(String customerId, String note) implements OrderEvent {}
public record AddItemEvent(String itemId, int quantity, BigDecimal price) implements OrderEvent {}
public record SubmitOrderEvent() implements OrderEvent {}
public record CancelOrderEvent(String reason) implements OrderEvent {}
```

## State Transition Evaluator

Use modern Java 21 pattern matching to handle transitions and enforce business rules (`BR-*`):

```java
public class OrderStateMachine {

    public static TransitionResult transition(OrderState currentState, OrderEvent event) {
        return switch (currentState) {
            case InitialState init when event instanceof CreateOrderEvent e -> {
                String orderId = UUID.randomUUID().toString();
                OrderState nextState = new DraftState(orderId, e.customerId(), new ArrayList<>());
                yield TransitionResult.success(nextState, "Order " + orderId + " created");
            }
            case DraftState draft when event instanceof AddItemEvent e -> {
                // Business rule check (e.g. BR-001: quantity must be positive)
                if (e.quantity() <= 0) {
                    yield TransitionResult.failure(draft, "BR-001: Quantity must be greater than zero");
                }
                List<OrderItem> updatedItems = new ArrayList<>(draft.items());
                updatedItems.add(new OrderItem(e.itemId(), e.quantity(), e.price()));
                yield TransitionResult.success(new DraftState(draft.orderId(), draft.customerId(), updatedItems), "Item added");
            }
            case DraftState draft when event instanceof SubmitOrderEvent -> {
                // Business rule check (e.g. BR-002: cannot submit empty order)
                if (draft.items().isEmpty()) {
                    yield TransitionResult.failure(draft, "BR-002: Cannot submit order with no items");
                }
                yield TransitionResult.success(new SubmittedState(draft.orderId(), draft.customerId(), draft.items(), Instant.now()), "Order submitted");
            }
            case SubmittedState sub when event instanceof CancelOrderEvent e -> {
                yield TransitionResult.success(new CancelledState(sub.orderId(), e.reason()), "Order cancelled");
            }
            default -> TransitionResult.failure(
                currentState,
                "Invalid transition: Cannot execute " + event.getClass().getSimpleName() + " in state " + currentState.name()
            );
        };
    }
}
```

