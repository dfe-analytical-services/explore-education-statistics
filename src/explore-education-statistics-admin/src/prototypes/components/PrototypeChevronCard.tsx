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
        <h3>
          <a href="#" className={styles.cardLink}>
            {title}
          </a>
        </h3>
        <p>{description}</p>
      </div>
    </li>
  );
};

export default ChevronCard;
