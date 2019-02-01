import { ComponentElement, ComponentType, ReactElement } from 'react';

export default function isComponentType<P>(
  value: unknown,
  componentType: ComponentType<P>,
): value is ComponentElement<P, any> {
  if (!value) {
    return false;
  }

  const element = value as ReactElement<any>;

  if (element.type !== undefined) {
    return (element.type as ComponentType<P>) === componentType;
  }

  return false;
}
