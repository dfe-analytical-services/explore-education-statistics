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
  type: 'mapped' | 'unmapped' | 'new' | 'noMappings';
  onClick?: (id: string) => void;
}

interface InnerProps {
  items: [Facet, Facet][];
  type: 'mapped' | 'unmapped' | 'new' | 'noMappings';
  onClick?: (id: string) => void;
}

const Inner = ({ items, type, onClick }: InnerProps) => (
  <div className={styles.container}>
    <div aria-hidden className="dfe-flex">
      <span className={styles.heading}>Current data set</span>
      <span className={styles.heading}>New data set</span>
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

const PrototypeFacetList = ({
  grouped,
  heading,
  items,
  type,
  onClick,
}: Props) => {
  if (grouped) {
    return (
      <Details open summary={heading}>
        <Inner items={items} type={type} onClick={onClick} />
      </Details>
    );
  }
  return <Inner items={items} type={type} onClick={onClick} />;
};
export default PrototypeFacetList;
