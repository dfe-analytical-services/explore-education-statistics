import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import InfoIcon from '@common/components/InfoIcon';
import React from 'react';

export default function DataModal() {
  return (
    <Modal
      showClose
      title="What is data?"
      triggerButton={
        <ButtonText>
          What is data? <InfoIcon description="Information on data" />
        </ButtonText>
      }
    >
      <p>
        Our data or ‘data sets’ are the underlying tables of numbers and
        categories that are used to create charts, tables and summaries which
        are used in the statistical articles.
      </p>
      <h3>The Structure of a ‘Data set’</h3>
      <p>
        Every data set is organised into two main components, referred to as
        Metadata:
      </p>
      <ul>
        <li>
          <strong>Indicators:</strong> These are the specific data points or
          measurements being collected.
          <ul>
            <li>
              Examples: Number of pupils, absence rates, percentage of students
              achieving a grade 5 or above, or total expenditure in pounds.
            </li>
          </ul>
        </li>
        <li>
          <strong>Filters:</strong> These allow you to break down the
          indicators.
          <ul>
            <li>
              Time Periods: e.g., Academic year 2022/23 or Calendar year 2023.
            </li>
            <li>
              Geographic Levels: Data can be viewed at a National, Regional, or
              Local Authority level, and sometimes down to individual school
              levels.
            </li>
            <li>
              Characteristics: These are specific filters like gender,
              ethnicity, free school meal eligibility, or school type (e.g.,
              academy vs. local authority maintained).
            </li>
          </ul>
        </li>
      </ul>
      <h3>How you can use this data</h3>
      <ul>
        <li>
          <strong>Data Catalogue:</strong> You can browse the Data Catalogue to
          see a list of every data set available. Each entry shows the number of
          rows, the geographic levels included, and the date it was last
          updated.
        </li>

        <li>
          <strong>Create tables:</strong> Select a data set and then use the
          "filters" to choose exactly which indicators and locations you want to
          see. The tool then generates a custom table for you.
        </li>

        <li>
          <strong>Download:</strong> You can download the entire data set as a
          CSV file. This is useful if you want to perform your own analysis in
          software like Excel, R, or Python.
        </li>
      </ul>
    </Modal>
  );
}
