import React, { ReactNode } from 'react';

export interface ButtonLinkProps {
  children: ReactNode;
  as: string;
  href: string;
  [key:string]: any;
}

let RealButtonLink : React.ComponentType<ButtonLinkProps>;
export type ButtonLinkType = typeof RealButtonLink;

export const SetRealButtonLink = (buttonLink : ButtonLinkType) => {
  RealButtonLink = buttonLink;
  return RealButtonLink;
};

const ButtonLink = (props: ButtonLinkProps) => {
  return (
    <RealButtonLink {...props} />
  );
};

export default ButtonLink;