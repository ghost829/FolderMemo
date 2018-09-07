using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FolderMemo
{
    public static class DEFINE
    {
        /// <summary>
        /// 트레이 아이콘명
        /// </summary>
        public const String TRAY_NAME = "FolderMemo";

        /// <summary>
        /// 프로그램 설명
        /// </summary>
        public const String MESSAGE_ABOUT = "간편한 메모앱 입니다. by SSW";

        /// <summary>
        /// Folder Form의 Defualt 크기
        /// </summary>
        public static System.Drawing.Size DEFAULT_FOLDER_SIZE = new System.Drawing.Size(420, 340);

        /// <summary>
        /// Memo Form의 Default 크기
        /// </summary>
        public static System.Drawing.Size DEFAULT_MEMO_SIZE = new System.Drawing.Size(280, 280);



        /// <summary>
        /// 설정 파일 명
        /// </summary>
        public const String CONFIG_FILENAME = "config.xml";

        /// <summary>
        /// 메모 데이터 정보가 들어있는 파일 명
        /// </summary>
        public const String MEMO_DATA_FILENAME = "data.xml";

        /// <summary>
        /// 메모 데이터 정보파일 전체경로(경로+파일명+확장자)
        /// 변수로 사용할꺼임
        /// </summary>
        public static String MEMO_DATA_PATH;

        /// <summary>
        /// Config파일 - TopMost 속성 명
        /// </summary>
        public const String CONFIG_SETTING_TOPMOST = "TopMost";

        /// <summary>
        /// Config파일 - Folder창이 닫힐때의 Rectangle
        /// </summary>
        public const String CONFIG_SETTING_CLOSERECT = "Rect";

        /// <summary>
        /// Config파일 - 메모 데이터 경로
        /// </summary>
        public const String CONFIG_SETTING_MEMODATAPATH = "MemoDataPath";

        /// <summary>
        /// XML 노드의 값이 문자열
        /// </summary>
        public const String NODETYPE_STRING = "0";

        /// <summary>
        /// Folder클래스의 리스트뷰에서 아이템에 마우스 오버시 출력되는 툴팁의 높이값
        /// </summary>
        public static int CUSTOMTOOLTIP_MAXIMUMHEIGHT = 400;

        /// <summary>
        /// Folder클래스의 리스트뷰에서 아이템에 마우스 오버시 출력되는 툴팁이 뜰때까지 기다릴 시간(초)
        /// </summary>
        public static float CUSTOMETOOLTIP_WAITTIME = 0.5f;

        /// <summary>
        /// Folder클래스의 리스트뷰에 출력되는 아이템의 타입
        /// </summary>
        public enum FILETYPE
        {
            FILETYPE_TEXT,      // 문서
            FILETYPE_DIRECTORY  // 디렉토리
        };

        /// <summary>
        /// 델리게이트 이벤트 TYPE
        /// </summary>
        public enum EVENTTYPE
        {
            EVENTTYPE_LOADMEMO,    // 메모 폼 생성 시
            EVENTTYPE_CLOSEMEMO,   // 메모 폼 닫힐때
            EVENTTYPE_SAVEMEMO,    // 메모 폼 저장 시
            EVENTTYPE_DELETEMEMO,  // 메모 폼 삭제 시
            EVENTTYPE_DELETEGROUP, // 그룹 삭제 시
            EVENTTYPE_RENAME,      // 이름 변경 시
            EVENTTYPE_SAVEMEMODATAPATH // 메모 데이터 경로 저장
        };

        /// <summary>
        /// 새로운 메모 생성 시 Prefix
        /// </summary>
        public const String PREFIX_MEMONAME = "memo";

        /// <summary>
        /// 새로운 그룹 생성 시 Prefix
        /// </summary>
        public const String PREFIX_GROUPNAME = "group";

        public const String NODE_MEMONAME = "MEMO";

        public const String NODE_GROUPNAME = "GROUP";
    }
}
