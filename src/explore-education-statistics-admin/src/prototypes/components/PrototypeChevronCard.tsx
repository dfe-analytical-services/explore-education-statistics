import classNames from 'classnames';
import React, { ReactNode } from 'react';
import styles from './PrototypeChevronCard.module.scss';

interface Props {
  className?: string;
  title?: string;
  description?: string;
}

const ChevronCard = ({ className, title, description }: Props) => {
  return (
    <li className={styles.card}>
      <div className={styles.wrapper}>
        <a
          href="#"
          className={classNames(styles.cardLink, 'govuk-!-margin-bottom-3')}
        >
          {title}
        </a>
        <p className="govuk-hint">{description}</p>
      </div>
    </li>
  );
};

export default ChevronCard;
