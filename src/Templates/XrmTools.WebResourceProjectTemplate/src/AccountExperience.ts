/**
 * A small Account form experience that demonstrates the shape of a form script.
 *
 * Add dist/AccountExperience.js as a form library, pass the execution context,
 * and use AccountExperience.onLoad as the form On Load handler.
 */
namespace AccountExperience {
    const notificationId = "account-experience-welcome";

    export function onLoad(executionContext: Xrm.Events.EventContext): void {
        const formContext = executionContext.getFormContext();

        showWelcomeMessage(formContext);

        // Keep the message current when the account name changes.
        formContext.getAttribute<Xrm.Attributes.StringAttribute>("name")
            ?.addOnChange(onNameChange);
    }

    export function onNameChange(executionContext: Xrm.Events.EventContext): void {
        showWelcomeMessage(executionContext.getFormContext());
    }

    function showWelcomeMessage(formContext: Xrm.FormContext): void {
        const accountName = formContext
            .getAttribute<Xrm.Attributes.StringAttribute>("name")
            ?.getValue()
            ?.trim();
        const createdOn = formContext
            .getAttribute<Xrm.Attributes.DateAttribute>("createdon")
            ?.getValue();

        const message = createdOn
            ? `${accountName || "This account"} has been with us for ${daysSince(createdOn)} days.`
            : `Welcome${accountName ? ` to ${accountName}` : ""}! Save the account to start its story.`;

        formContext.ui.setFormNotification(message, "INFO", notificationId);
    }

    function daysSince(date: Date): number {
        const millisecondsPerDay = 24 * 60 * 60 * 1000;
        return Math.max(0, Math.floor((Date.now() - date.getTime()) / millisecondsPerDay));
    }
}
