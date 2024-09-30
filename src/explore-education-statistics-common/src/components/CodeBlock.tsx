import styles from '@common/components/CodeBlock.module.scss';
import useToggle from '@common/hooks/useToggle';
import React, { useEffect } from 'react';
import bashLang from 'react-syntax-highlighter/dist/cjs/languages/hljs/bash';
import pythonLang from 'react-syntax-highlighter/dist/cjs/languages/hljs/python';
import rLang from 'react-syntax-highlighter/dist/cjs/languages/hljs/r';
import SyntaxHighlighter from 'react-syntax-highlighter/dist/cjs/light';
import Button from './Button';

SyntaxHighlighter.registerLanguage('bash', bashLang);
SyntaxHighlighter.registerLanguage('r', rLang);
SyntaxHighlighter.registerLanguage('python', pythonLang);

export interface CodeBlockProps {
  children: string;
  language: 'bash' | 'python' | 'r';
}

export default function CodeBlock({ children, language }: CodeBlockProps) {
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
          await navigator.clipboard.writeText(children);
          toggleCopied.on();
        }}
      >
        <span aria-live="polite">{copied ? 'Code copied' : 'Copy code'}</span>
      </Button>

      <SyntaxHighlighter
        className={styles.pre}
        codeTagProps={{ tabIndex: 0 }}
        language={language}
        useInlineStyles={false}
      >
        {children}
      </SyntaxHighlighter>
    </div>
  );
}
