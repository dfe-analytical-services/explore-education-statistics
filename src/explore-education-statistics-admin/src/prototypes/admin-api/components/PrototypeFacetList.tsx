import Details from '@common/components/Details';
import React from 'react';
import PrototypeFacet from './PrototypeFacet';
import styles from './PrototypeFacetList.module.scss';

export interface Facet {
  label: string;
  caption?: string;
  id?: string;
}

interface Props {
  grouped: boolean;
  heading: string;
  items: [Facet, Facet][];
  type: 'mapped' | 'unmapped' | 'new';
  onClick?: (id: string) => void;
}

const PrototypeFacetList = ({
  grouped,
  heading,
  items,
  type,
  onClick,
}: Props) => {
  const Inner = () => (
    <div className={styles.container}>
      <div aria-hidden className="dfe-flex">
        <span className={styles.heading}>Current dataset</span>
        <span className={styles.heading}>New dataset</span>
      </div>
      {items.map((item, index) => {
        return (
          <PrototypeFacet
            item={item}
            // eslint-disable-next-line react/no-array-index-key
            key={index}
            itemType={type}
            onClick={onClick}
          />
        );
      })}
    </div>
  );

  if (grouped) {
    return (
      <Details open summary={heading}>
        <Inner />
      </Details>
    );
  }
  return <Inner />;
};
export default PrototypeFacetList;
