import Button from '@common/components/Button';
import Modal from '@common/components/Modal';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import { ThemeSummary } from '@common/services/themeService';
import styles from '@frontend/modules/find-statistics/components/FilterModals.module.scss';
import React from 'react';

export const ReleaseTypesModal = ({
  open,
  onClose,
}: {
  open: boolean;
  onClose: () => void;
}) => (
  <Modal
    className="govuk-!-width-one-half"
    open={open}
    title="Release types guidance"
    onExit={onClose}
  >
    <p>
      This is a description list of the different types of publication we
      currently publish.
    </p>
    {/* eslint-disable-next-line jsx-a11y/no-noninteractive-tabindex */}
    <div className={styles.content} tabIndex={0}>
      <SummaryList>
        <SummaryListItem term="National statistics">
          National statistics are official statistics that have been assessed by
          the Office for Statistics Regulation as fully compliant with the Code
          of Practice for Statistics, i.e., they meet the highest standards for
          trustworthiness, quality and value.
        </SummaryListItem>
        <SummaryListItem term="Official statistics">
          Official statistics are regular statistics produced by the UK
          Statistics Authority, government departments (including executive
          agencies), the Devolved Administrations in Scotland, Wales and
          Northern Ireland, any other person acting on behalf of the Crown or
          any other organisation named on an Official Statistics Order.
        </SummaryListItem>
        <SummaryListItem term="Experimental statistics">
          Experimental statistics are newly developed or innovative official
          statistics that are undergoing evaluation. They are published to
          involve users and stakeholders in the assessment of their suitability
          and quality at an early stage. These statistics will reach a point
          where the label, experimental statistics, can be removed, or should be
          discontinued.
        </SummaryListItem>
        <SummaryListItem term="Ad hoc statistics">
          Releases of statistics which are not part of DfE's regular annual
          official statistical release calendar.
        </SummaryListItem>
        <SummaryListItem term="Management information (MI)">
          Management information describes aggregate information collated and
          used in the normal course of business to inform operational delivery,
          policy development or the management of organisational performance. It
          is usually based on administrative data but can also be a product of
          survey data. The terms administrative data and management information
          are sometimes used interchangeably.
        </SummaryListItem>
      </SummaryList>
    </div>
    <Button className="govuk-!-margin-bottom-0" onClick={onClose}>
      Close
    </Button>
  </Modal>
);

export const ThemesModal = ({
  open,
  themes,
  onClose,
}: {
  open: boolean;
  themes: ThemeSummary[];
  onClose: () => void;
}) => (
  <Modal
    className="govuk-!-width-one-half"
    open={open}
    title="Themes guidance"
    onExit={onClose}
  >
    <p>This is a description list of our different publication themes.</p>
    {/* eslint-disable-next-line jsx-a11y/no-noninteractive-tabindex */}
    <div className={styles.content} tabIndex={0}>
      <SummaryList>
        {themes.map(theme => (
          <SummaryListItem key={theme.id} term={theme.title}>
            {theme.summary}
          </SummaryListItem>
        ))}
      </SummaryList>
    </div>
    <Button className="govuk-!-margin-bottom-0" onClick={onClose}>
      Close
    </Button>
  </Modal>
);
