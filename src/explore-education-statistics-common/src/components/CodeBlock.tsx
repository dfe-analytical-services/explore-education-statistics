import styles from '@common/components/Code.module.scss';
import React, { useEffect } from 'react';
import { Highlight, Prism, PrismTheme } from 'prism-react-renderer';
import useToggle from '@common/hooks/useToggle';
import Button from './Button';

(typeof global !== 'undefined' ? global : window).Prism = Prism;
require('prismjs/components/prism-r');
require('prismjs/components/prism-python');

const themeGovUk: PrismTheme = {
  plain: { color: '#0a0a3f' },
  styles: [
    {
      types: ['comment'],
      style: { color: '#545555', fontStyle: 'italic' },
    },
    {
      types: ['quote'],
      style: { color: '#545555', fontStyle: 'italic' },
    },
    {
      types: ['keyword', 'selector-tag', 'subst'],
      style: { color: '#333333', fontWeight: 'bold' },
    },
    {
      types: ['number', 'literal', 'template-variable', 'tag', 'attr'],
      style: { color: '#00703c' },
    },
    {
      types: ['string', 'doctag'],
      style: { color: '#d13118' },
    },
    {
      types: ['title', 'section', 'selector-id'],
      style: { color: '#99000', fontWeight: 'bold' },
    },
    {
      types: ['subst'],
      style: { fontWeight: 'normal' },
    },
    {
      types: ['type', 'class', 'class-name', 'maybe-class-name', 'title'],
      style: { color: '#445588', fontWeight: 'bold' },
    },
    {
      types: ['name', 'tag', 'attribute', 'attr-name'],
      style: { color: '#003078', fontWeight: 'normal' },
    },
    {
      types: ['regexp', 'link'],
      style: { color: '#008020' },
    },
    {
      types: ['symbol', 'bullet'],
      style: { color: '#990073' },
    },
    {
      types: ['builtin', 'builtin-name'],
      style: { color: '#017ba5' },
    },
    {
      types: ['meta'],
      style: { color: '#545555', fontWeight: 'bold' },
    },
    {
      types: ['deletion'],
      style: { background: '#ffdddd' },
    },
    {
      types: ['addition'],
      style: { background: '#ddffdd' },
    },
    {
      types: ['emphasis'],
      style: { fontStyle: 'italic' },
    },
    {
      types: ['strong'],
      style: { fontWeight: 'bold' },
    },
  ],
};

export interface CodeBlockProps {
  language?: string;
  code: string;
}

export default function CodeBlock({
  code,
  language = 'python',
}: CodeBlockProps) {
  const [copied, toggleCopied] = useToggle(false);

  useEffect(() => {
    const resetTimeout = setTimeout(toggleCopied.off, 5000);

    return () => {
      if (copied === true) {
        clearTimeout(resetTimeout);
      }
    };
  }, [copied, toggleCopied]);

  return (
    <Highlight prism={Prism} theme={themeGovUk} code={code} language={language}>
      {({ style, tokens, getLineProps, getTokenProps }) => (
        <div className={styles.container}>
          <Button
            className={styles.copyButton}
            onClick={async () => {
              await navigator.clipboard.writeText(code);
              toggleCopied.on();
            }}
          >
            {copied ? <span aria-live="polite">Code copied</span> : 'Copy Code'}
          </Button>
          {/* eslint-disable-next-line */}
          <pre tabIndex={0} className={styles.pre} style={style}>
            <code>
              {tokens.map((line, i) => (
                /* eslint-disable-next-line */
                <div key={String(`ln:${i}`)} {...getLineProps({ line })}>
                  {line.map((token, key) => (
                    /* eslint-disable-next-line */
                    <span key={key} {...getTokenProps({ token })} />
                  ))}
                </div>
              ))}
            </code>
          </pre>
        </div>
      )}
    </Highlight>
  );
}
