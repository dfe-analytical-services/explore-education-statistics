import React, { ReactNode } from 'react';


interface LinkAdminProps {
  children: ReactNode;
  to: string;
}

interface LinkFrontEndProps {
  children: ReactNode;
  to?: string;
  as: string;
  href: string;
}

export type ButtonLinkProps = (LinkAdminProps | LinkFrontEndProps);

let RealButtonLink : React.ComponentType<{}>;
export type ButtonLinkType = typeof RealButtonLink;

export function SetRealButtonLink<T>(buttonLink : T) {
  RealButtonLink = buttonLink;
  return RealButtonLink;
};

const ButtonLink = (props: ButtonLinkProps) => {
  return (
    <RealButtonLink {...props} />
  );
};

export default ButtonLink;