import React, { ReactNode } from 'react';

interface Props {
  children: ReactNode;
}

const VisuallyHidden = ({ children }: Props) => {
  return <span className="govuk-visually-hidden">{children}</span>;
};

export default VisuallyHidden;
