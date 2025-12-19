import React, { ReactNode, type JSX } from 'react';

interface Props {
  children: ReactNode;
  as?: keyof JSX.IntrinsicElements;
}

const VisuallyHidden = ({ as: Element = 'span', children }: Props) => {
  return <Element className="govuk-visually-hidden">{children}</Element>;
};

export default VisuallyHidden;
