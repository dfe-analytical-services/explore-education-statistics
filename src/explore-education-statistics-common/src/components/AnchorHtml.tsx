import { OmitStrict } from '@common/types';
import applyRedirectRules from '@common/utils/url/applyRedirectRules';
import React, { AnchorHTMLAttributes, useEffect, useState } from 'react';

interface AnchorHtmlProps
  extends OmitStrict<AnchorHTMLAttributes<HTMLAnchorElement>, 'href'> {
  href: string;
}

export default function AnchorHtml({
  href,
  target,
  rel,
  children,
}: AnchorHtmlProps) {
  const [sanitisedHref, setSanitisedHref] = useState<string | null>(null);

  useEffect(() => {
    const sanitiseHref = async (rawHref: string) => {
      setSanitisedHref(await applyRedirectRules(rawHref));
    };

    sanitiseHref(href);
  }, [href]);

  if (!sanitisedHref) {
    return <a href={undefined}>[link loading...]</a>;
  }

  return (
    <a href={sanitisedHref} target={target} rel={rel}>
      {children}
    </a>
  );
}
