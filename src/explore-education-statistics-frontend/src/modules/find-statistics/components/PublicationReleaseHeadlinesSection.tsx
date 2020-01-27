import React from 'react';
import { Release } from '@common/services/publicationService';

type Props = Pick<
  Release,
  'keyStatisticsSection' | 'keyStatisticsSecondarySection' | 'headlinesSection'
>;

const HeadlinesSection = ({
  keyStatisticsSection,
  headlinesSection,
  keyStatisticsSecondarySection,
}: Props) => {
  return <>HeadlinesSection</>;
};

export default HeadlinesSection;
