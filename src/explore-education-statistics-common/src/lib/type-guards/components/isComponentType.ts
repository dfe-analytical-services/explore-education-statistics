import { Component, ComponentElement, ComponentState, ComponentType, ReactElement } from 'react';

export default function isComponentType<P>(
  value: unknown,
  componentType: ComponentType<P>,
): value is ComponentElement<P, Component<P, ComponentState>> {
  if (!value) {
    return false;
  }

  const element = value as ReactElement<P>;

  if (element.type !== undefined) {
    return element.type === componentType;
  }

  return false;
}
