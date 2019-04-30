import Details, { DetailsProps } from '@common/components/Details';
import styles from '@common/components/DetailsMenu.module.scss';
import classNames from 'classnames';
import React from 'react';

function DetailsMenu({ children, className, ...props }: DetailsProps) {
  return (
    <Details {...props} className={classNames(styles.details, className)}>
      {children}
    </Details>
  );
}

export default DetailsMenu;
