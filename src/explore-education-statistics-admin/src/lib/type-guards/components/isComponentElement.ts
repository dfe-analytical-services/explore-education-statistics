import { Component, ComponentType, ReactElement } from 'react';

export default function isComponentElement<P>(
  value: unknown,
): value is ComponentType<P> {
  if (!value) {
    return false;
  }

  const element = value as ReactElement<any>;

  if (element.type !== undefined) {
    return (
      (element.type as any) instanceof Component ||
      typeof element.type === 'function'
    );
  }

  return false;
}
