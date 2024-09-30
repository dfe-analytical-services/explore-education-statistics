import styles from '@common/components/Code.module.scss';
import useToggle from '@common/hooks/useToggle';
import React, { useEffect } from 'react';
import pythonLang from 'react-syntax-highlighter/dist/cjs/languages/hljs/python';
import rLang from 'react-syntax-highlighter/dist/cjs/languages/hljs/r';
import SyntaxHighlighter from 'react-syntax-highlighter/dist/cjs/light';
import a11yLight from 'react-syntax-highlighter/dist/cjs/styles/hljs/a11y-light';
import Button from './Button';

SyntaxHighlighter.registerLanguage('r', rLang);
SyntaxHighlighter.registerLanguage('python', pythonLang);

export interface CodeBlockProps {
  language: 'python' | 'r';
  code: string;
}

export default function CodeBlock({ code, language }: CodeBlockProps) {
  const [copied, toggleCopied] = useToggle(false);

  useEffect(() => {
    const resetTimeout = setTimeout(toggleCopied.off, 5000);

    return () => {
      if (copied) {
        clearTimeout(resetTimeout);
      }
    };
  }, [copied, toggleCopied]);

  return (
    <div className={styles.container}>
      <Button
        className={styles.copyButton}
        onClick={async () => {
          await navigator.clipboard.writeText(code);
          toggleCopied.on();
        }}
      >
        {copied ? <span aria-live="polite">Code copied</span> : 'Copy code'}
      </Button>
      <SyntaxHighlighter
        className={styles.pre}
        language={language}
        style={a11yLight}
        codeTagProps={{ tabIndex: 0 }}
      >
        {code}
      </SyntaxHighlighter>
    </div>
  );
}
