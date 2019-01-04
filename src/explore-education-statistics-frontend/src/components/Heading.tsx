import classNames from 'classnames';
import React, { HTMLAttributes, ReactNode } from 'react';

type HeadingElementProps = {
  children: ReactNode;
} & HTMLAttributes<HTMLHeadingElement>;

type Props = {
  size: 'xl' | 'l' | 'm' | 's';
} & HeadingElementProps;

const headingTags = {
  l: 'h2',
  m: 'h3',
  s: 'h4',
  xl: 'h1',
};

const Heading = ({ children, size, ...props }: Props) =>
  React.createElement<HTMLAttributes<HTMLHeadingElement>>(
    headingTags[size],
    {
      ...props,
      className: classNames(props.className, `govuk-heading-${size}`),
    },
    children,
  );

export default Heading;

export const H1 = ({ children, ...props }: HeadingElementProps) => (
  <Heading {...props} size="xl">
    {children}
  </Heading>
);

export const H2 = ({ children, ...props }: HeadingElementProps) => (
  <Heading {...props} size="l">
    {children}
  </Heading>
);

export const H3 = ({ children, ...props }: HeadingElementProps) => (
  <Heading {...props} size="m">
    {children}
  </Heading>
);

export const H4 = ({ children, ...props }: HeadingElementProps) => (
  <Heading {...props} size="s">
    {children}
  </Heading>
);
