import styles from '@frontend/modules/table-tool/components/ShortlistedSearchResultItem.module.scss';
import React from 'react';

interface Props {
  relevance: number;
  title: string;
}

const ShortlistedSearchResultItem = ({ relevance, title }: Props) => (
  <li className={styles.item}>
    <span>{title}</span>
    <span>{relevance}% relevance</span>
  </li>
);

export default ShortlistedSearchResultItem;
