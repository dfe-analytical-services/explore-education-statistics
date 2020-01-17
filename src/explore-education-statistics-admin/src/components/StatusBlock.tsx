import React from 'react';
import classNames from 'classnames';
import styles from '@admin/pages/release/edit-release/data/ReleaseDataUploadsSection.module.scss';

interface Props {
  className?: string;
  color: string | undefined;
  text: string;
}

const StatusBlock = ({ color, text, className }: Props) => {
  const colorClass = () => {
    switch (color) {
      case 'orange':
        return [styles.ragStatusAmber];
      case 'red':
        return [styles.ragStatusRed];
      case 'green':
        return [styles.ragStatusGreen];
      default:
        return undefined;
    }
  };

  return (
    <>
      {/* this strong block could be a StatusBlock component that takes color/className prop(s) and renders the text children */}
      <strong className={classNames('govuk-tag', colorClass(), className)}>
        {text}
      </strong>
      {/* show loading spinner if timer is active (it's still processing) */}
    </>
  );
};

export default StatusBlock;
