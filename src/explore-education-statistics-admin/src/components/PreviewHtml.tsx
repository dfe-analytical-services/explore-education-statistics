import SanitizeHtml from '@common/components/SanitizeHtml';
import classNames from 'classnames';
import React from 'react';
import styles from './PreviewHtml.module.scss';

interface Props {
  className?: string;
  html: string;
  testId?: string;
}

const PreviewHtml = ({ className, html, testId }: Props) => {
  return (
    <SanitizeHtml
      dirtyHtml={html}
      className={classNames(styles.preview, className)}
      testId={testId}
    />
  );
};

export default PreviewHtml;
