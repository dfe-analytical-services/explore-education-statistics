import VisuallyHidden from '@common/components/VisuallyHidden';
import styles from '@frontend/components/Feedback.module.scss';
import Button from '@common/components/Button';
import classNames from 'classnames';
import { useMemo, useState } from 'react';
import FormProvider from '@common/components/form/FormProvider';
import { Form, FormFieldTextArea } from '@common/components/form';
import ButtonGroup from '@common/components/ButtonGroup';
import feedbackService from '@frontend/services/feedbackService';
import {
  FeedbackRequest,
  FeedbackResponse,
} from '@common/services/types/feedback';
import Yup from '@common/validation/yup';
import { ObjectSchema } from 'yup';

type BannerState = 'initial' | 'notUseful' | 'problemEncountered' | 'thanks';

interface FormValues {
  context?: string;
  issue?: string;
  intent?: string;
}

const feedbackLimit = 2000;

export default function Feedback() {
  const [bannerState, setBannerState] = useState<BannerState>('initial');
  const [response, setResponse] = useState<FeedbackResponse | undefined>();

  const handleUsefulFeedback = async () => {
    await feedbackService.sendFeedback({
      url: window.location.pathname,
      userAgent: navigator.userAgent,
      response: 'Useful',
    });

    setBannerState('thanks');
  };

  const cancel = () => {
    setResponse(undefined);
    setBannerState('initial');
  };

  const handleSubmit = async (values: FeedbackRequest) => {
    await feedbackService.sendFeedback({
      url: window.location.pathname,
      userAgent: navigator.userAgent,
      response: response as FeedbackResponse,
      context: values.context,
      issue: values.issue,
      intent: values.intent,
    });

    setBannerState('thanks');
  };

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      context: Yup.string().max(
        feedbackLimit,
        `What were you doing must be ${feedbackLimit} characters or less`,
      ),
      issue: Yup.string().max(
        feedbackLimit,
        `What went wrong must  be ${feedbackLimit} characters or less`,
      ),
      intent: Yup.string().max(
        feedbackLimit,
        `What were you hoping to achieve must be ${feedbackLimit} characters or less`,
      ),
    });
  }, []);

  return (
    <div className={classNames('govuk-width-container', styles.container)}>
      <div className={styles.banner}>
        {bannerState === 'initial' && (
          <>
            <div className={styles.promptQuestion}>
              <h2 className={styles.heading}>Is this page useful?</h2>
              <Button
                className={styles.buttonYesNo}
                variant="secondary"
                onClick={handleUsefulFeedback}
              >
                Yes<VisuallyHidden> this page is useful</VisuallyHidden>
              </Button>

              <Button
                className={styles.buttonYesNo}
                variant="secondary"
                ariaControls="feedbackFormContainer"
                ariaExpanded={response === 'NotUseful'}
                onClick={() => {
                  setResponse('NotUseful');
                  setBannerState('notUseful');
                }}
              >
                No<VisuallyHidden> this page is not useful</VisuallyHidden>
              </Button>
            </div>

            <div className={styles.reportContainer}>
              <Button
                className={styles.buttonReport}
                variant="secondary"
                ariaControls="feedbackFormContainer"
                ariaExpanded={response === 'ProblemEncountered'}
                onClick={() => {
                  setResponse('ProblemEncountered');
                  setBannerState('problemEncountered');
                }}
              >
                Report a problem with this page
              </Button>
            </div>
          </>
        )}

        {bannerState === 'thanks' && (
          <div className={styles.promptQuestion} role="alert">
            <p className={styles.heading}>Thank you for your feedback</p>
          </div>
        )}
      </div>

      <div
        id="feedbackFormContainer"
        className="govuk-!-padding-left-6 govuk-!-padding-right-6"
        hidden={bannerState === 'initial' || bannerState === 'thanks'}
      >
        <h3 className="govuk-!-margin-top-5">
          Help us improve Explore education statistics
        </h3>
        <p>
          Don't include personal or financial information like your National
          Insurance number or credit card details.
        </p>

        <FormProvider validationSchema={validationSchema}>
          {({ formState }) => {
            return (
              <Form id="feedbackForm" onSubmit={handleSubmit}>
                {response && (
                  <FormFieldTextArea<FeedbackRequest>
                    label="What were you doing?"
                    name="context"
                    rows={3}
                    maxLength={2000}
                  />
                )}

                {response === 'NotUseful' && (
                  <FormFieldTextArea<FeedbackRequest>
                    label="What were you hoping to achieve?"
                    name="intent"
                    rows={3}
                    maxLength={2000}
                  />
                )}

                {response === 'ProblemEncountered' && (
                  <FormFieldTextArea<FeedbackRequest>
                    label="What went wrong?"
                    name="issue"
                    rows={3}
                    maxLength={2000}
                  />
                )}

                <ButtonGroup>
                  <Button type="submit" disabled={formState.isSubmitting}>
                    Send
                  </Button>
                  <Button variant="secondary" onClick={cancel}>
                    Cancel
                  </Button>
                </ButtonGroup>
              </Form>
            );
          }}
        </FormProvider>
      </div>
    </div>
  );
}
