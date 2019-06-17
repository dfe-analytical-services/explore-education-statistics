import React, {
  AnchorHTMLAttributes,
  FunctionComponent,
  ReactNode,
} from 'react';

type Props = {
  className?: string;
  to: string;
  children: ReactNode;
} & AnchorHTMLAttributes<HTMLAnchorElement>;

type RendererType = FunctionComponent<Props>;

// some default renderer
let Renderer: FunctionComponent<Props> = ({ children }: Props) => {
  return (
    <span>
      <em>Link Renderer Not Defined</em>
      {children}
    </span>
  );
};

const Link: FunctionComponent<Props> = (props: Props) => {
  return <Renderer {...props} />;
};

export function RegisterLinkRenderer<Props>(component: RendererType) {
  Renderer = component;
  return component;
}

export default Link;
