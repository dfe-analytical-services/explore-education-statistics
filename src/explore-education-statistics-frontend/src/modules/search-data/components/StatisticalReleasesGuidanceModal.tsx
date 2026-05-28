import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import InfoIcon from '@common/components/InfoIcon';
import React from 'react';

export default function StatisticalReleasesGuidanceModal() {
  return (
    <Modal
      showClose
      title="What are statistical releases?"
      triggerButton={
        <ButtonText>
          What are statistical releases?{' '}
          <InfoIcon description="Information on statistical releases" />
        </ButtonText>
      }
    >
      <p>
        Search and browse statistical releases, these include summaries and also
        allow you to download associated data to help you understand and analyse
        our range of statistics.
      </p>
      <p>
        A <strong>statistical release</strong> page within EES provides a
        collection of insights, tables, charts and direct access to data on a
        given education subject, ranging from early years childcare to higher
        education outcomes and school workforce statistics.
      </p>
      <p>
        Most statistical releases are published as part of a release series of
        yearly, termly or fortnightly releases.
      </p>
      <p>
        When you open a specific statistical release, the page is structured to
        help you digest complex data through several key sections:
      </p>
      <ul>
        <li>
          <strong>Headline Facts and Figures:</strong> A high-level summary of
          the most important numbers (e.g., total number of pupils).
        </li>
        <li>
          <strong>Narrative and Commentary:</strong> Written analysis by
          Department for Education (DfE) statisticians explaining the "why"
          behind the numbers and highlighting significant trends.
        </li>
        <li>
          <strong>Explore Data:</strong> A tool that allows you to "build your
          own tables." You can filter the underlying data by geography (e.g.,
          local authority), school type, or pupil characteristics.
        </li>
        <li>
          <strong>Methodology:</strong> Documentation explaining how the data
          was collected, any changes in definitions, and how to interpret the
          quality of the statistics.
        </li>
      </ul>
    </Modal>
  );
}
