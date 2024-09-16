import styles from '@common/components/Code.module.scss';
import useToggle from '@common/hooks/useToggle';
import React, { useEffect } from 'react';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import Button from './Button';

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
      <SyntaxHighlighter language={language} className={styles.pre}>
        {code}
      </SyntaxHighlighter>
    </div>
  );
}
