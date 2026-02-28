<template>
  <PageFrame>
    <div class="assignment-page">
      <h2>{{ t('assignment.title') }}</h2>

      <div v-if="assignmentContent" class="assignment-body">
        <p class="assignment-text">{{ assignmentContent.text }}</p>
      </div>

      <div class="section">
        <h3>{{ t('assignment.grade') }}</h3>
        <div v-if="submission">
          <p v-if="submission.grade !== null && submission.grade !== undefined">
            {{ submission.grade }}
          </p>
          <p v-else class="empty-msg">{{ t('assignment.notGraded') }}</p>
          <div class="submission-content" v-for="c in submission.contents" :key="c.text">
            <p>{{ c.text }}</p>
          </div>
        </div>
        <p v-else class="empty-msg">{{ t('assignment.notGraded') }}</p>
      </div>

      <div class="section">
        <h3>{{ t('assignment.submit') }}</h3>
        <textarea v-model="answerText" class="answer-textarea" :placeholder="t('assignment.text')" rows="6" />
        <p v-if="submitMsg" class="msg">{{ submitMsg }}</p>
        <button class="round-button s-main h" @click="submit">{{ t('btn.submit') }}</button>
      </div>
    </div>
  </PageFrame>
  <PageFooter />
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import PageFrame from '@/components/PageFrame.vue';
import PageFooter from '@/components/lesser/PageFooter.vue';
import { getClassInstanceInfo, ContentItem } from '@/api/class-instance';
import { getMySubmission, submitAssignment, AssignmentSubmission } from '@/api/assignment';

const { t } = useI18n();
const route = useRoute();

const instanceId = route.params.instanceId as string;
const contentId = route.params.contentId as string;

const assignmentContent = ref<ContentItem | null>(null);
const submission = ref<AssignmentSubmission | null>(null);
const answerText = ref('');
const submitMsg = ref('');

onMounted(async () => {
  const instRes = await getClassInstanceInfo(instanceId);
  if (instRes.status === 200 && instRes.result) {
    assignmentContent.value = instRes.result.contents.find(c => c.id === contentId) ?? null;
  }

  const subRes = await getMySubmission(instanceId, contentId);
  if (subRes.status === 200 && subRes.result) {
    submission.value = subRes.result;
    answerText.value = subRes.result.contents[0]?.text ?? '';
  }
});

async function submit() {
  if (!answerText.value.trim()) return;
  const res = await submitAssignment(instanceId, contentId, answerText.value);
  submitMsg.value = res.status === 200 ? t('status.submitted') : `${t('status.error')}${res.info}`;
  if (res.status === 200) {
    const subRes = await getMySubmission(instanceId, contentId);
    if (subRes.status === 200 && subRes.result) submission.value = subRes.result;
  }
}
</script>

<style scoped>
.assignment-page { padding: 24px; }
.assignment-body { margin: 16px 0; }
.assignment-text { white-space: pre-wrap; }
.section { margin-top: 24px; }
.section h3 { margin-bottom: 8px; }
.empty-msg { color: var(--color-txt3); }
.answer-textarea { width: 100%; padding: 10px; border: 1px solid #ccc; border-radius: 4px; background: transparent; color: var(--color-txt1); resize: vertical; box-sizing: border-box; margin-bottom: 10px; }
.submission-content { background: var(--color-bg-trs); padding: 10px; border-radius: 4px; margin-top: 8px; }
.msg { color: var(--color-txt3); margin: 6px 0; }
</style>
