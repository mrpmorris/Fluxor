# Fluxor - documentation

## Flux pattern

Often confused with Redux. Redux is the name of a library, Flux is the name of the pattern that Redux and
Fluxor implement.

![](./../Images/flux-pattern.jpg)

**Rules**
* State should always be read-only.
* To alter state our app should dispatch an action.
* Every reducer that processes the dispatched action type will create new state to reflect the old
state combined with the changes expected for the action.
* The UI then uses the new state to render its display.

## Tutorials
### Basic concepts

* [State, actions, and reducers](../Tutorials/01-BasicConcepts/01A-StateActionsReducersTutorial/README.md)
* [Effects](../Tutorials/01-BasicConcepts/01B-EffectsTutorial/README.md)
* [Middleware](../Tutorials/01-BasicConcepts/01C-MiddlewareTutorial/README.md)
* [ActionSubscriber](../Tutorials/01-BasicConcepts/01E-ActionSubscriber/README.md)

### Blazor for web

* [State, actions, and reducers](../Tutorials/02-Blazor/02A-StateActionsReducersTutorial/README.md)
* [Effects](../Tutorials/02-Blazor/02B-EffectsTutorial/README.md)
* [Middleware](../Tutorials/02-Blazor/02C-MiddlewareTutorial/README.md)
* [Redux Dev Tools](../Tutorials/02-Blazor/02D-ReduxDevToolsTutorial/README.md)
