import Button from '@common/components/Button';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import PrototypeDataTilesHighlights from '@common/prototypes/publication/components/PrototypeDataTilesHighlights';
import React, { useState } from 'react';
import { FormGroup, FormSelect } from '@common/components/form';
import PrototypeAdminEmbedTables from '@admin/pages/prototypes/components/PrototypeAdminEmbedTables';
import PrototypeEditableContent from '@admin/pages/prototypes/components/PrototypeEditableContent';
import PrototypeChartSample from '@admin/pages/prototypes/publication/components/PrototypeChartSample';
import PrototypeTableSample from '@common/prototypes/publication/components/PrototypeTableSample';

interface Props {
  editing?: boolean;
  reviewing?: boolean;
  sectionId?: string;
  chartTitle?: string;
  xAxisLabel?: string;
  yAxisLabel?: string;
  chartData?: object[];
  chartDataKeys: string[];
}

const PrototypeExampleTable = () => {
  const [value, setValue] = useState('');

  const [addDatablock, setAddDataBlock] = useState(false);

  return (
    <React.Fragment>
      {!addDatablock && (
        <button
          type="button"
          className="govuk-button"
          onClick={() => setAddDataBlock(true)}
        >
          Add / change data block
        </button>
      )}
      {addDatablock && (
        <>
          <FormGroup>
            <FormSelect
              id="select-table"
              name="select-table"
              label="Select a data block"
              value={value}
              onChange={event => {
                setValue(event.target.value);
              }}
              options={[
                {
                  label: 'Select a data block',
                  value: '',
                },
                {
                  label: 'Absence by characteristic table with chart',
                  value: 'table-1',
                },
              ]}
            />
          </FormGroup>
          {value === '' && (
            <button
              type="button"
              className="govuk-button govuk-button--secondary"
              onClick={() => {
                setValue('');
                setAddDataBlock(false);
              }}
            >
              Cancel
            </button>
          )}
        </>
      )}
      {addDatablock && value && value !== '' && (
        <>
          <PrototypeAdminEmbedTables />
        </>
      )}
      {addDatablock && value && value !== '' && (
        <React.Fragment>
          <button
            className="govuk-button govuk-!-margin-right-6"
            type="button"
            onClick={() => setAddDataBlock(false)}
          >
            Embed
          </button>
          <button
            type="button"
            className="govuk-button govuk-button--secondary"
            onClick={() => {
              setValue('');
              setAddDataBlock(false);
            }}
          >
            Cancel
          </button>
        </React.Fragment>
      )}
    </React.Fragment>
  );
};

const PrototypeDataSample = ({
  editing,
  reviewing,
  sectionId,
  chartTitle,
  xAxisLabel,
  yAxisLabel,
  chartData,
  chartDataKeys,
}: Props) => {
  return (
    <>
      {editing && <PrototypeExampleTable />}
      <Tabs id="data-sample-tabs">
        {sectionId === 'headlines' && (
          <TabsSection id={`${sectionId}SummaryData`} title="Summary">
            <PrototypeDataTilesHighlights editing={editing} />
            {editing && <Button>Add another key indicator</Button>}
            <PrototypeEditableContent
              editable={editing}
              reviewing={reviewing}
              content={`
            <ul className="govuk-list govuk-list--bullet">
              <li>pupils missed on average 8.2 school days</li>
              <li>
                overall and unauthorised absence rates up on previous year
              </li>
              <li>
                unauthorised rise due to higher rates of unauthorised holidays
              </li>
              <li>10% of pupils persistently absent during 2016/17</li>
            </ul>
          `}
            />
          </TabsSection>
        )}

        <TabsSection id={`${sectionId}TableData`} title="Table">
          <PrototypeTableSample
            caption={`Table showing ${chartTitle}`}
            editing={editing}
          />
        </TabsSection>
        <TabsSection id={`${sectionId}ChartData`} title="Chart">
          <h2 className="govuk-heading-s">{`Chart showing ${chartTitle}`}</h2>
          <PrototypeChartSample
            xAxisLabel={xAxisLabel}
            yAxisLabel={yAxisLabel}
            chartData={chartData}
            chartDataKeys={chartDataKeys}
          />
        </TabsSection>
      </Tabs>
    </>
  );
};

export default PrototypeDataSample;
