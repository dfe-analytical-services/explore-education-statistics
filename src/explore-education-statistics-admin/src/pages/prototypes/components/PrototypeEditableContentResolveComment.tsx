import React from 'react';
import Details from '@common/components/Details';
import {
  FormGroup,
  FormTextInput,
  FormSelect,
  FormRadioGroup,
} from '@common/components/form';

import styles from './PrototypeEditableContentAddComment.module.scss';

interface Props {
  name?: string;
  date?: string;
}

const ContentResolveComment = ({ name, date }: Props) => {
  return (
    <>
      <div className={styles.resolveComment}>
        <Details summary="View comments" className="govuk-!-margin-bottom-1">
          <form>
            <h2 className="govuk-body-s">{name}</h2>
            <p className="govuk-body-s">
              Lorem ipsum dolor sit, amet consectetur adipisicing elit. Tempore,
              nostrum atque assumenda vitae quae ratione nihil accusamus
              provident natus ipsum iure numquam ex labore nam, deleniti impedit
              repudiandae eligendi quasi.
            </p>
            <button type="submit" className="govuk-button">
              Resolve
            </button>
          </form>
        </Details>
      </div>
    </>
  );
};

export default ContentResolveComment;
