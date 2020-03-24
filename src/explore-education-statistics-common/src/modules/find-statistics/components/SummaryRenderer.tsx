import React from 'react';
import KeyStatTile, { KeyStatProps } from './KeyStatTile';
import styles from './SummaryRenderer.module.scss';

interface SummaryRendererProps {
  dataBlocks: KeyStatProps[];
}

export default function SummaryRenderer({ dataBlocks }: SummaryRendererProps) {
  return (
    <>
      <div className={styles.keyStatsContainer}>
        {dataBlocks.map(dataBlock => {
          return <KeyStatTile key={dataBlock.id} {...dataBlock} />;
        })}
      </div>
    </>
  );
}
