import { ComponentType, ReactComponentElement, ReactElement } from 'react';

export default function isComponentType<P>(
  value: unknown,
  componentType: ComponentType<P>,
): value is ReactComponentElement<ComponentType<P>, P> {
  if (!value) {
    return false;
  }

  const element = value as ReactElement<P>;

  if (element.type !== undefined) {
    return element.type === componentType;
  }

  return false;
}
