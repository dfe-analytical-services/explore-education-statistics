import React from 'react';
import { Result } from '@common/services/dataBlockService';
import styles from './SummaryRenderer.module.scss';
import KeyStatTile, { KeyStatProps } from './KeyStatTile';

interface SummaryRendererProps {
  datablocks: KeyStatProps[];
}

export function getLatestMeasures(result: Result[]) {
  const copiedResult = [...result];

  copiedResult.sort((a, b) => a.timePeriod.localeCompare(b.timePeriod));

  const latest = copiedResult[copiedResult.length - 1];

  const { measures } = latest;
  return measures;
}

export default function SummaryRenderer({ datablocks }: SummaryRendererProps) {
  return (
    <>
      <div className={styles.keyStatsContainer}>
        {datablocks.map(datablock => {
          return <KeyStatTile key={datablock.id} {...datablock} />;
        })}
      </div>
    </>
  );
}
