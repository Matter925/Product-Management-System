export interface QuestionDto {
    id: number;
    title: string;
    type: string;
    for: string;
    isParent: boolean;
    isUpdated: boolean;
    answerQuestions: AnswerDto[];
    childQuestions :QuestionDto[];
  }
  
  export interface AnswerDto {
    id: number;
    questionId: number;
    text: string;
    isFinal: boolean;
    isUpdated: boolean;
    nextQuestionId?: number | null;
    nextQuestion?: QuestionDto | null;
  }
  
  export interface ResponseCreateDto {
    ClientId: number;
    CreatedAt: Date;
    Question: string;
    Answer: string;
    nextQuestionId: number;
    questionId: number;
  }