import React from 'react';
import classNames from 'classnames';
import { Facet } from './PrototypeFacetList';
import styles from './PrototypeFacet.module.scss';

interface Props {
  item: [Facet, Facet];
  itemType: 'mapped' | 'unmapped' | 'new' | 'noMappings';
  onClick?: (id: string) => void;
}

const PrototypeFacet = ({ item, itemType, onClick }: Props) => {
  const Inner = () => (
    <>
      <span className={styles.label}>Current data set </span>
      <span
        className={classNames(styles.map, {
          [styles.mapDisableOption]: itemType === 'new',
        })}
      >
        {itemType === 'new' && <p>Not applicable to current data set</p>}
        {item[0].label}
        {item[0].caption && (
          <span className={styles.caption}> {item[0].caption}</span>
        )}{' '}
      </span>
      <span className={styles.divider} />
      <span className={styles.label}>Next data set </span>
      <span className={styles.map}>
        {item[1].label}
        {item[1].caption && (
          <span className={styles.caption}> {item[1].caption}</span>
        )}
      </span>
    </>
  );

  if (
    itemType === 'unmapped' ||
    itemType === 'mapped' ||
    itemType === 'noMappings'
  ) {
    return (
      <button
        onClick={() => {
          if (item[0].id) {
            onClick?.(item[0].id);
          }
        }}
        type="button"
        className={classNames(styles.item, {
          [styles.itemWarning]: itemType === 'unmapped',
        })}
      >
        <Inner />
      </button>
    );
  }

  return (
    <div className={styles.item}>
      <Inner />
    </div>
  );
};
export default PrototypeFacet;
