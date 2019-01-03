import React, { ReactNode } from 'react';
import { Link } from 'react-router-dom';

interface Props {
  children: ReactNode,
}

const Glink = ({ children }: Props) => (
  <Link to="#" className="govuk-link govuk-link--no-visited-state">{children}</Link>
);

export default Glink;
