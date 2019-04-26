import React from 'react';

interface Props {
  children: React.ReactNode;
}

export default function MethodologyContent({ children }: Props) {
  return <div className="govuk-grid-column-three-quarters">{children}</div>;
}
