import { OmitStrict } from '@common/types';
import React, { ComponentType, ReactNode } from 'react';
import LazyLoad, { LazyLoadProps as BaseLazyLoadProps } from 'react-lazyload';

interface LazyLoadProps<P> extends OmitStrict<BaseLazyLoadProps, 'children'> {
  placeholder?: ((props: P) => ReactNode) | ReactNode;
}

export default function withLazyLoad<P extends object>(
  Component: ComponentType<P>,
  lazyLoadProps: LazyLoadProps<P> = {
    once: true,
  },
) {
  const LazyLoadedComponent = (props: P) => {
    const placeholder =
      typeof lazyLoadProps.placeholder === 'function'
        ? lazyLoadProps.placeholder(props)
        : lazyLoadProps.placeholder;

    return (
      <LazyLoad {...lazyLoadProps} placeholder={placeholder}>
        <Component {...props} />
      </LazyLoad>
    );
  };

  LazyLoadedComponent.displayName = `withLazyLoad(${
    Component.displayName || Component.name
  })`;

  return LazyLoadedComponent;
}
