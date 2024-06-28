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

  // TODO: Discuss how we want to achieve this in PR review
  // One option is to make this a hook
  // Another is to make this call during frontend's GetServerSideProps and expose to common through a context or something
  // Another is to leave it like this - but there's still a question of what to render until the API has responded
  // Also, can we handle caching a bit more smartly?
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
